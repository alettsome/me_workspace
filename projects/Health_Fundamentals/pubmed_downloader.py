"""
PubMed Central (PMC) Article Downloader + Summarizer
Search and download full-text scientific articles on health/nutrition
"""
import asyncio
import aiohttp
import json
from pathlib import Path
from datetime import datetime
from xml.etree import ElementTree as ET

class PubMedDownloader:
    def __init__(self, config_path="config.json"):
        with open(config_path) as f:
            config = json.load(f)
        
        self.deepseek_api_key = config.get("deepseek_api_key")
        self.deepseek_model = config.get("deepseek_model", "deepseek-chat")
        self.output_base = Path("Research/pubmed_summaries")
        self.output_base.mkdir(parents=True, exist_ok=True)
        
        # PubMed Central API endpoints
        self.search_url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi"
        self.fetch_url = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi"
    
    async def search_articles(self, query: str, max_results: int = 20):
        """Search PubMed Central for open-access articles"""
        print(f"🔍 Searching PubMed Central: '{query}'...")
        
        params = {
            'db': 'pmc',  # PubMed Central (full-text articles)
            'term': f'{query} free full text',  # Search with free full text filter
            'retmax': max_results,
            'retmode': 'json',
            'sort': 'relevance',
            'tool': 'health_research',
            'email': 'research@localhost'
        }
        
        async with aiohttp.ClientSession() as session:
            try:
                async with session.get(self.search_url, params=params, timeout=30) as response:
                    if response.status == 200:
                        data = await response.json()
                        id_list = data.get('esearchresult', {}).get('idlist', [])
                        count = data.get('esearchresult', {}).get('count', 0)
                        print(f"   ✅ Found {count} articles, returning {len(id_list)} IDs")
                        return id_list
                    else:
                        print(f"   ❌ Search failed: {response.status}")
                        return []
            except Exception as e:
                print(f"   ❌ Error: {type(e).__name__}: {str(e)}")
                return []
    
    async def fetch_article_text(self, pmc_id: str):
        """Fetch full article text from PubMed Central"""
        print(f"  📥 Downloading PMC article {pmc_id}...")
        
        params = {
            'db': 'pmc',
            'id': pmc_id,
            'retmode': 'xml'
        }
        
        async with aiohttp.ClientSession() as session:
            try:
                async with session.get(self.fetch_url, params=params, timeout=60) as response:
                    if response.status == 200:
                        xml_text = await response.text()
                        
                        # Parse XML to extract title, abstract, and body text
                        root = ET.fromstring(xml_text)
                        
                        # Extract title
                        title_elem = root.find('.//article-title')
                        title = title_elem.text if title_elem is not None else "Unknown Title"
                        
                        # Extract abstract
                        abstract = ""
                        for abs_elem in root.findall('.//abstract//p'):
                            if abs_elem.text:
                                abstract += abs_elem.text + "\n\n"
                        
                        # Extract body text
                        body_text = ""
                        for p in root.findall('.//body//p'):
                            if p.text:
                                body_text += p.text + "\n\n"
                            # Also get text from child elements
                            for child in p.iter():
                                if child.text and child.tag not in ['xref', 'sup']:
                                    body_text += child.text
                        
                        full_text = f"Title: {title}\n\n"
                        if abstract:
                            full_text += f"Abstract:\n{abstract}\n"
                        if body_text:
                            full_text += f"Full Text:\n{body_text}"
                        
                        print(f"     ✅ Downloaded {len(full_text):,} characters")
                        return {
                            'pmc_id': pmc_id,
                            'title': title,
                            'text': full_text
                        }
                    else:
                        print(f"     ❌ Download failed: {response.status}")
                        return None
            except Exception as e:
                print(f"     ❌ Error: {type(e).__name__}: {str(e)[:100]}")
                return None
    
    async def summarize_with_deepseek(self, article: dict):
        """Summarize article using DeepSeek API"""
        print(f"  🤖 Summarizing with DeepSeek...")
        
        text = article['text']
        title = article['title']
        pmc_id = article['pmc_id']
        
        # Truncate if needed
        if len(text) > 100000:
            text = text[:100000]
        
        prompt = f"""You are analyzing the scientific article "{title}" (PMC{pmc_id}) for health and nutrition findings.

Please provide a structured summary with these sections:

1. **Research Question/Hypothesis**: What did the study investigate?
2. **Key Findings**: Main results related to health, nutrition, or diet
3. **Evidence Strength**: Study design, sample size, methodology quality
4. **Clinical Implications**: What does this mean for health recommendations?
5. **Limitations**: What are the study's limitations or caveats?
6. **Notable Quotes**: 2-3 key excerpts that capture the findings

Article text:
{text}

Provide a clear, evidence-focused summary suitable for citing in health education materials."""

        url = "https://api.deepseek.com/v1/chat/completions"
        headers = {
            "Authorization": f"Bearer {self.deepseek_api_key}",
            "Content-Type": "application/json"
        }
        
        payload = {
            "model": self.deepseek_model,
            "messages": [{"role": "user", "content": prompt}],
            "temperature": 0.3,
            "max_tokens": 2000
        }
        
        async with aiohttp.ClientSession() as session:
            try:
                async with session.post(url, json=payload, headers=headers, timeout=60) as response:
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
    
    async def process_article(self, pmc_id: str):
        """Download and summarize a single article"""
        print(f"\n{'='*60}")
        print(f"📄 Processing PMC Article {pmc_id}")
        print(f"{'='*60}")
        
        # Check if already processed
        output_file = self.output_base / f"PMC{pmc_id}_summary.md"
        if output_file.exists():
            print(f"  ⏭️  Already processed")
            return {'pmc_id': pmc_id, 'status': 'skipped'}
        
        # Fetch article
        article = await self.fetch_article_text(pmc_id)
        if not article:
            return {'pmc_id': pmc_id, 'status': 'failed', 'reason': 'download_failed'}
        
        # Summarize
        summary = await self.summarize_with_deepseek(article)
        if not summary:
            return {'pmc_id': pmc_id, 'status': 'failed', 'reason': 'summarization_failed'}
        
        # Save summary
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(f"# {article['title']}\n\n")
            f.write(f"**Source:** PubMed Central  \n")
            f.write(f"**PMC ID:** PMC{pmc_id}  \n")
            f.write(f"**URL:** https://www.ncbi.nlm.nih.gov/pmc/articles/PMC{pmc_id}/  \n")
            f.write(f"**Processed:** {datetime.utcnow().isoformat()}  \n")
            f.write(f"**Article Length:** {len(article['text']):,} characters  \n\n")
            f.write("---\n\n")
            f.write(summary)
            f.write("\n\n---\n\n")
            f.write("**Ethical Note:** This summary was generated from an open-access scientific article. ")
            f.write(f"Full text available at: https://www.ncbi.nlm.nih.gov/pmc/articles/PMC{pmc_id}/\n")
        
        print(f"  ✅ Summary saved: {output_file.name}")
        return {'pmc_id': pmc_id, 'status': 'success', 'title': article['title']}
    
    async def process_batch(self, article_ids: list, batch_size: int = 3):
        """Process multiple articles with controlled parallelism"""
        print(f"\n{'='*60}")
        print(f"📚 Processing {len(article_ids)} articles ({batch_size} at a time)")
        print(f"{'='*60}\n")
        
        results = []
        for i in range(0, len(article_ids), batch_size):
            batch = article_ids[i:i + batch_size]
            print(f"\n🔄 Batch {i//batch_size + 1}: Processing {len(batch)} articles...\n")
            
            tasks = [self.process_article(pmc_id) for pmc_id in batch]
            batch_results = await asyncio.gather(*tasks, return_exceptions=True)
            
            for result in batch_results:
                if isinstance(result, Exception):
                    results.append({'status': 'error', 'error': str(result)})
                else:
                    results.append(result)
            
            # Brief pause between batches
            if i + batch_size < len(article_ids):
                await asyncio.sleep(2)
        
        # Summary
        successful = sum(1 for r in results if r.get('status') == 'success')
        failed = sum(1 for r in results if r.get('status') == 'failed')
        skipped = sum(1 for r in results if r.get('status') == 'skipped')
        
        print(f"\n{'='*60}")
        print(f"📊 BATCH COMPLETE")
        print(f"{'='*60}")
        print(f"✅ Successful: {successful}/{len(article_ids)}")
        print(f"⏭️  Skipped: {skipped}/{len(article_ids)}")
        print(f"❌ Failed: {failed}/{len(article_ids)}")
        
        return results

async def main():
    import argparse
    parser = argparse.ArgumentParser(description='Download and summarize PubMed Central articles')
    parser.add_argument('--query', type=str, default='nutrition health diet intervention', 
                       help='Search query')
    parser.add_argument('--count', type=int, default=5, help='Number of articles to process')
    parser.add_argument('--batch-size', type=int, default=3, help='Concurrent downloads')
    
    args = parser.parse_args()
    
    downloader = PubMedDownloader('config.json')
    
    # Search for articles
    article_ids = await downloader.search_articles(args.query, args.count)
    
    if not article_ids:
        print("❌ No articles found")
        return
    
    # Process articles
    await downloader.process_batch(article_ids[:args.count], args.batch_size)

if __name__ == '__main__':
    asyncio.run(main())
