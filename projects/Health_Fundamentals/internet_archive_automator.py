"""
Internet Archive Book Automation
Automatically borrows, captures pages, and returns books from Internet Archive
"""
import asyncio
import json
import sys
from pathlib import Path
from datetime import datetime
from playwright.async_api import async_playwright, TimeoutError as PlaywrightTimeout

class InternetArchiveAutomator:
    def __init__(self, config_path="config.json"):
        self.config = self._load_config(config_path)
        self.output_base = Path(self.config.get("output_dir", "Research/01-Inbox"))
        self.output_base.mkdir(parents=True, exist_ok=True)
        
    def _load_config(self, config_path):
        """Load configuration or use defaults"""
        try:
            with open(config_path, 'r') as f:
                return json.load(f)
        except FileNotFoundError:
            return {
                "output_dir": "Research/01-Inbox",
                "screenshot_delay_ms": 1500,
                "page_load_timeout_ms": 10000,
                "headless": False
            }
    
    async def process_book(self, isbn: str, page_ranges: list, title: str = "", author: str = ""):
        """
        Borrow book from Internet Archive, capture specified pages, return book
        
        Args:
            isbn: Internet Archive identifier (not always actual ISBN)
            page_ranges: List of tuples, e.g., [(1, 50), (100, 150)]
            title: Book title for logging
            author: Book author for logging
        """
        print(f"\n{'='*60}")
        print(f"Processing: {title or isbn}")
        if author:
            print(f"Author: {author}")
        print(f"{'='*60}\n")
        
        output_dir = self.output_base / isbn
        output_dir.mkdir(parents=True, exist_ok=True)
        
        async with async_playwright() as p:
            browser = await p.chromium.launch(
                headless=self.config.get("headless", False),
                args=['--start-maximized']
            )
            
            context = await browser.new_context(
                viewport={'width': 1920, 'height': 1080},
                user_agent='Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
            )
            
            page = await context.new_page()
            
            try:
                # Navigate to book page
                url = f"https://archive.org/details/{isbn}"
                print(f"📖 Opening: {url}")
                await page.goto(url, wait_until='domcontentloaded')
                await asyncio.sleep(2)
                
                # Check if book is available
                if await self._check_availability(page):
                    print("✅ Book is available")
                else:
                    print("❌ Book not available for borrowing")
                    await browser.close()
                    return False
                
                # Borrow the book
                borrowed = await self._borrow_book(page)
                if not borrowed:
                    print("❌ Failed to borrow book")
                    await browser.close()
                    return False
                
                print("✅ Book borrowed successfully")
                await asyncio.sleep(3)
                
                # Open book reader
                reader_opened = await self._open_reader(page)
                if not reader_opened:
                    print("❌ Failed to open book reader")
                    await self._return_book(page, url)
                    await browser.close()
                    return False
                
                print("✅ Book reader opened")
                await asyncio.sleep(4)
                
                # Capture pages
                total_pages = sum(end - start + 1 for start, end in page_ranges)
                captured = 0
                
                for range_idx, (start_page, end_page) in enumerate(page_ranges):
                    print(f"\n📸 Capturing range {range_idx + 1}/{len(page_ranges)}: pages {start_page}-{end_page}")
                    
                    for page_num in range(start_page, end_page + 1):
                        success = await self._capture_page(page, page_num, output_dir)
                        if success:
                            captured += 1
                            print(f"  ✓ Page {page_num} ({captured}/{total_pages})")
                        else:
                            print(f"  ✗ Failed: page {page_num}")
                        
                        await asyncio.sleep(self.config.get("screenshot_delay_ms", 1500) / 1000)
                
                print(f"\n✅ Captured {captured}/{total_pages} pages")
                
                # Save metadata
                metadata = {
                    "isbn": isbn,
                    "title": title,
                    "author": author,
                    "processed_utc": datetime.utcnow().isoformat(),
                    "page_ranges": page_ranges,
                    "pages_captured": captured,
                    "source": "Internet Archive",
                    "rights_label": "summary-only"
                }
                
                metadata_path = output_dir / "metadata.json"
                with open(metadata_path, 'w', encoding='utf-8') as f:
                    json.dump(metadata, f, indent=2)
                
                print(f"💾 Metadata saved: {metadata_path}")
                
                # Return the book
                await page.goto(url)
                await asyncio.sleep(2)
                await self._return_book(page, url)
                print("✅ Book returned")
                
                await browser.close()
                return True
                
            except Exception as e:
                print(f"❌ Error: {e}")
                await browser.close()
                return False
    
    async def _check_availability(self, page):
        """Check if book is available for borrowing"""
        try:
            # Look for borrow button or "Read" link
            borrow_button = await page.query_selector('button:has-text("Borrow"), a.borrow-link')
            return borrow_button is not None
        except:
            return False
    
    async def _borrow_book(self, page):
        """Click borrow button"""
        try:
            # Try different selectors for borrow button
            selectors = [
                'button:has-text("Borrow")',
                'a.borrow-link',
                '.borrow-button',
                '[data-action="borrow"]'
            ]
            
            for selector in selectors:
                try:
                    await page.click(selector, timeout=3000)
                    await asyncio.sleep(2)
                    return True
                except:
                    continue
            
            return False
        except Exception as e:
            print(f"Borrow error: {e}")
            return False
    
    async def _open_reader(self, page):
        """Open the book reader interface"""
        try:
            # Try different selectors for read button
            selectors = [
                'a:has-text("Read")',
                '.read-button',
                '[data-action="read"]',
                'a[href*="stream"]'
            ]
            
            for selector in selectors:
                try:
                    await page.click(selector, timeout=3000)
                    await asyncio.sleep(3)
                    return True
                except:
                    continue
            
            return False
        except Exception as e:
            print(f"Reader open error: {e}")
            return False
    
    async def _capture_page(self, page, page_num: int, output_dir: Path):
        """Navigate to specific page and capture screenshot"""
        try:
            # Wait for BookReader to be ready
            await page.wait_for_selector('.BRcontainer', timeout=5000)
            
            # Try to navigate to page using BookReader API
            await page.evaluate(f'if (window.br) {{ br.jumpToIndex({page_num - 1}); }}')
            await asyncio.sleep(1.5)
            
            # Take screenshot of the book content area
            screenshot_path = output_dir / f"page-{page_num:04d}.png"
            
            # Try to screenshot just the book image
            book_image = await page.query_selector('.BRpageimage')
            if book_image:
                await book_image.screenshot(path=str(screenshot_path))
            else:
                # Fallback to full page
                await page.screenshot(path=str(screenshot_path))
            
            return True
            
        except Exception as e:
            print(f"  Capture error on page {page_num}: {e}")
            return False
    
    async def _return_book(self, page, book_url: str):
        """Return the borrowed book"""
        try:
            await page.goto(book_url)
            await asyncio.sleep(2)
            
            # Try different selectors for return button
            selectors = [
                'button:has-text("Return")',
                'a:has-text("Return")',
                '.return-button',
                '[data-action="return"]'
            ]
            
            for selector in selectors:
                try:
                    await page.click(selector, timeout=3000)
                    await asyncio.sleep(1)
                    return True
                except:
                    continue
            
            return False
        except:
            return False
    
    async def process_batch(self, books: list):
        """
        Process multiple books in sequence
        
        Args:
            books: List of dicts with keys: isbn, title, author, page_ranges
        """
        print(f"\n{'='*60}")
        print(f"BATCH PROCESSING: {len(books)} books")
        print(f"{'='*60}\n")
        
        results = []
        for idx, book in enumerate(books, 1):
            print(f"\n[{idx}/{len(books)}] Starting: {book.get('title', book['isbn'])}")
            
            success = await self.process_book(
                isbn=book['isbn'],
                page_ranges=book['page_ranges'],
                title=book.get('title', ''),
                author=book.get('author', '')
            )
            
            results.append({
                'isbn': book['isbn'],
                'title': book.get('title', ''),
                'success': success
            })
            
            # Rate limiting between books
            if idx < len(books):
                print(f"\n⏳ Waiting 60 seconds before next book...")
                await asyncio.sleep(60)
        
        # Print summary
        print(f"\n{'='*60}")
        print("BATCH COMPLETE")
        print(f"{'='*60}")
        successful = sum(1 for r in results if r['success'])
        print(f"✅ Successful: {successful}/{len(books)}")
        print(f"❌ Failed: {len(books) - successful}/{len(books)}")
        
        if successful < len(books):
            print("\nFailed books:")
            for r in results:
                if not r['success']:
                    print(f"  - {r['title'] or r['isbn']}")


def main():
    """Command line interface"""
    import argparse
    
    parser = argparse.ArgumentParser(description='Automate Internet Archive book capture')
    parser.add_argument('--isbn', help='Internet Archive book identifier')
    parser.add_argument('--title', help='Book title', default='')
    parser.add_argument('--author', help='Book author', default='')
    parser.add_argument('--pages', help='Page ranges (e.g., "1-50,100-150")')
    parser.add_argument('--batch', help='Path to JSON file with multiple books')
    parser.add_argument('--config', help='Path to config.json', default='config.json')
    
    args = parser.parse_args()
    
    automator = InternetArchiveAutomator(args.config)
    
    if args.batch:
        # Batch processing
        with open(args.batch, 'r') as f:
            books = json.load(f)
        asyncio.run(automator.process_batch(books))
    
    elif args.isbn and args.pages:
        # Single book processing
        page_ranges = []
        for range_str in args.pages.split(','):
            start, end = map(int, range_str.split('-'))
            page_ranges.append((start, end))
        
        asyncio.run(automator.process_book(
            isbn=args.isbn,
            page_ranges=page_ranges,
            title=args.title,
            author=args.author
        ))
    
    else:
        parser.print_help()
        sys.exit(1)


if __name__ == '__main__':
    main()
