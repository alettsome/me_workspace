"""
Project Gutenberg Health Books Downloader + Summarizer
Downloads curated health books and creates summaries using DeepSeek
"""
import asyncio
import aiohttp
import json
from pathlib import Path
from datetime import datetime
from gutenberg_real_health_books import books

class GutenbergDownloader:
    def __init__(self, config_path="config.json"):
        with open(config_path) as f:
            config = json.load(f)
        
        self.deepseek_api_key = config.get("deepseek_api_key")
        self.deepseek_model = config.get("deepseek_model", "deepseek-chat")
        self.output_base = Path("Research/gutenberg_summaries")
        self.output_base.mkdir(parents=True, exist_ok=True)
    
    async def download_book_text(self, book_id: int, title: str):
        """Download plain text from Project Gutenberg"""
        # Try multiple URL patterns
        urls = [
            f"https://www.gutenberg.org/cache/epub/{book_id}/pg{book_id}.txt",
            f"https://www.gutenberg.org/files/{book_id}/{book_id}-0.txt",
            f"https://www.gutenberg.org/files/{book_id}/{book_id}.txt"
        ]
        
        print(f"  📥 Downloading book {book_id}...")
        
        async with aiohttp.ClientSession() as session:
            for url in urls:
                try:
                    async with session.get(url, timeout=aiohttp.ClientTimeout(total=30)) as response:
                        if response.status == 200:
                            text = await response.text(encoding='utf-8', errors='ignore')
                            if len(text) > 1000:  # Must have substantial content
                                print(f"     ✅ Downloaded {len(text):,} characters from {url}")
                                return text
                except Exception as e:
                    print(f"     ⏭️  {url} - {type(e).__name__}")
                    continue
        
        return None
    
    def clean_gutenberg_text(self, text: str):
        """Remove Project Gutenberg header/footer"""
        # Find start of actual content
        start_markers = [
            "*** START OF THIS PROJECT GUTENBERG",
            "*** START OF THE PROJECT GUTENBERG",
            "*END*THE SMALL PRINT"
        ]
        
        for marker in start_markers:
            if marker in text:
                text = text.split(marker, 1)[1]
                break
        
        # Find end of actual content
        end_markers = [
            "*** END OF THIS PROJECT GUTENBERG",
            "*** END OF THE PROJECT GUTENBERG",
            "End of the Project Gutenberg"
        ]
        
        for marker in end_markers:
            if marker in text:
                text = text.split(marker, 1)[0]
                break
        
        return text.strip()
    
    async def summarize_with_deepseek(self, text: str, title: str, author: str, book_id: int):
        """Summarize book using DeepSeek API"""
        print(f"  🤖 Summarizing with DeepSeek...")
        
        # Truncate to manageable size (DeepSeek handles ~100k chars)
        if len(text) > 100000:
            text = text[:100000]
        
        prompt = f"""You are analyzing the book "{title}" by {author} for health and nutrition claims.

Please provide a structured summary with these sections:

1. **Key Health Claims**: List the main health-related assertions made in the book
2. **Dietary Recommendations**: Specific foods, nutrients, or eating patterns recommended
3. **Evidence Cited**: What evidence or reasoning does the author provide (if any)
4. **Core Themes**: Major topics and perspectives
5. **Notable Quotes**: 2-3 memorable passages that capture the book's message

Book text:
{text}

Provide a clear, well-organized summary that would help someone understand the book's health perspective."""

        url = "https://api.deepseek.com/v1/chat/completions"
        headers = {
            "Authorization": f"Bearer {self.deepseek_api_key}",
            "Content-Type": "application/json"
        }
        
        payload = {
            "model": self.deepseek_model,
            "messages": [
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.3,
            "max_tokens": 2000
        }
        
        async with aiohttp.ClientSession() as session:
            try:
                async with session.post(url, json=payload, headers=headers, timeout=aiohttp.ClientTimeout(total=60)) as response:
                    if response.status == 200:
                        result = await response.json()
                        summary = result['choices'][0]['message']['content']
                        print(f"     ✅ Summary generated ({len(summary)} characters)")
                        return summary
                    else:
                        error_text = await response.text()
                        print(f"     ❌ DeepSeek error {response.status}: {error_text[:200]}")
                        return None
            except Exception as e:
                print(f"     ❌ Error: {type(e).__name__}: {str(e)[:100]}")
                return None
    
    async def process_book(self, book_info: dict):
        """Download and summarize a single book"""
        book_id = book_info['id']
        title = book_info['title']
        author = book_info['author']
        
        print(f"\n{'='*60}")
        print(f"📖 {title}")
        print(f"   by {author} (PG #{book_id})")
        print(f"{'='*60}")
        
        # Check if already processed
        output_file = self.output_base / f"pg{book_id}_{title[:30].replace(' ', '_')}_summary.md"
        if output_file.exists():
            print(f"  ⏭️  Already processed (summary exists)")
            return {'id': book_id, 'title': title, 'status': 'skipped', 'reason': 'already_exists'}
        
        # Download text
        text = await self.download_book_text(book_id, title)
        if not text:
            print(f"  ❌ Download failed")
            return {'id': book_id, 'title': title, 'status': 'failed', 'reason': 'download_failed'}
        
        # Clean text
        clean_text = self.clean_gutenberg_text(text)
        print(f"  🧹 Cleaned text: {len(clean_text):,} characters")
        
        # Summarize
        summary = await self.summarize_with_deepseek(clean_text, title, author, book_id)
        if not summary:
            print(f"  ❌ Summarization failed")
            return {'id': book_id, 'title': title, 'status': 'failed', 'reason': 'summarization_failed'}
        
        # Save summary
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(f"# {title}\n\n")
            f.write(f"**Author:** {author}  \n")
            f.write(f"**Source:** Project Gutenberg (Book #{book_id})  \n")
            f.write(f"**Processed:** {datetime.utcnow().isoformat()}  \n")
            f.write(f"**Original Length:** {len(text):,} characters  \n")
            f.write(f"**Cleaned Length:** {len(clean_text):,} characters  \n\n")
            f.write("---\n\n")
            f.write(summary)
            f.write("\n\n---\n\n")
            f.write("**Ethical Note:** This summary was generated from a public domain book available on Project Gutenberg. ")
            f.write("Full text was not stored per retention policy. ")
            f.write(f"Original available at: https://www.gutenberg.org/ebooks/{book_id}\n")
        
        print(f"  ✅ Summary saved: {output_file.name}")
        return {'id': book_id, 'title': title, 'status': 'success'}
    
    async def process_batch(self, book_list: list, batch_size: int = 5):
        """Process multiple books with controlled parallelism"""
        print(f"\n{'='*60}")
        print(f"📚 Processing {len(book_list)} books ({batch_size} at a time)")
        print(f"{'='*60}\n")
        
        results = []
        for i in range(0, len(book_list), batch_size):
            batch = book_list[i:i + batch_size]
            print(f"\n🔄 Batch {i//batch_size + 1}: Processing {len(batch)} books...\n")
            
            tasks = [self.process_book(book) for book in batch]
            batch_results = await asyncio.gather(*tasks, return_exceptions=True)
            
            for result in batch_results:
                if isinstance(result, Exception):
                    results.append({'status': 'error', 'error': str(result)})
                else:
                    results.append(result)
            
            # Brief pause between batches
            if i + batch_size < len(book_list):
                await asyncio.sleep(2)
        
        # Summary
        successful = sum(1 for r in results if r.get('status') == 'success')
        failed = sum(1 for r in results if r.get('status') == 'failed')
        skipped = sum(1 for r in results if r.get('status') == 'skipped')
        
        print(f"\n{'='*60}")
        print(f"📊 BATCH COMPLETE")
        print(f"{'='*60}")
        print(f"✅ Successful: {successful}/{len(book_list)}")
        print(f"⏭️  Skipped: {skipped}/{len(book_list)}")
        print(f"❌ Failed: {failed}/{len(book_list)}")
        
        return results

async def main():
    import argparse
    parser = argparse.ArgumentParser(description='Download and summarize Project Gutenberg health books')
    parser.add_argument('--count', type=int, default=5, help='Number of books to process (default: 5)')
    parser.add_argument('--batch-size', type=int, default=3, help='Concurrent downloads (default: 3)')
    parser.add_argument('--all', action='store_true', help='Process all books in the list')
    
    args = parser.parse_args()
    
    # Select books to process
    books_to_process = books[:args.count] if not args.all else books
    
    downloader = GutenbergDownloader('config.json')
    await downloader.process_batch(books_to_process, args.batch_size)

if __name__ == '__main__':
    asyncio.run(main())
