"""
Pattern Detection Tool for Health Fundamentals Book
Extracts health claims from chapters and finds supporting evidence from research summaries
"""
import json
import re
from pathlib import Path
from typing import List, Dict
import asyncio
import aiohttp

class PatternDetector:
    def __init__(self, config_path="config.json"):
        with open(config_path) as f:
            config = json.load(f)
        
        self.deepseek_api_key = config.get("deepseek_api_key")
        self.deepseek_model = config.get("deepseek_model", "deepseek-chat")
        
        # Paths to summary folders
        self.gutenberg_path = Path("Research/gutenberg_summaries")
        self.pubmed_path = Path("Research/pubmed_summaries")
        
        # Load all summaries
        self.summaries = []
        self._load_summaries()
    
    def _load_summaries(self):
        """Load all research summaries"""
        print("📚 Loading research summaries...")
        
        # Load Gutenberg summaries
        if self.gutenberg_path.exists():
            for file in self.gutenberg_path.glob("*.md"):
                with open(file, 'r', encoding='utf-8') as f:
                    content = f.read()
                    self.summaries.append({
                        'source': 'gutenberg',
                        'file': file.name,
                        'content': content
                    })
        
        # Load PubMed summaries
        if self.pubmed_path.exists():
            for file in self.pubmed_path.glob("*.md"):
                with open(file, 'r', encoding='utf-8') as f:
                    content = f.read()
                    self.summaries.append({
                        'source': 'pubmed',
                        'file': file.name,
                        'content': content
                    })
        
        print(f"   ✅ Loaded {len(self.summaries)} summaries")
        print(f"      - {sum(1 for s in self.summaries if s['source'] == 'gutenberg')} from Project Gutenberg")
        print(f"      - {sum(1 for s in self.summaries if s['source'] == 'pubmed')} from PubMed Central")
    
    async def extract_chapter_claims(self, chapter_file: Path):
        """Extract key health claims from a chapter using DeepSeek"""
        print(f"\n🔍 Extracting health claims from {chapter_file.name}...")
        
        with open(chapter_file, 'r', encoding='utf-8') as f:
            chapter_text = f.read()
        
        # Truncate if needed
        if len(chapter_text) > 100000:
            chapter_text = chapter_text[:100000]
        
        prompt = f"""You are analyzing a health book chapter to extract specific, verifiable health claims.

Read this chapter and extract ALL specific health claims, nutritional facts, and biological mechanisms mentioned.

For each claim, provide:
1. The exact claim (be specific)
2. The category (e.g., Hydration, Minerals, Vitamins, Fiber, Proteins, Fats, etc.)
3. Why it matters (the health outcome or mechanism)

Format as a numbered list. Be thorough - extract EVERY claim, even small ones.

Example format:
1. **Claim:** Mild dehydration affects mood, energy, and concentration
   **Category:** Hydration
   **Why it matters:** Body function depends on adequate water for cellular processes

Chapter text:
{chapter_text}

Extract all health claims now:"""

        url = "https://api.deepseek.com/v1/chat/completions"
        headers = {
            "Authorization": f"Bearer {self.deepseek_api_key}",
            "Content-Type": "application/json"
        }
        
        payload = {
            "model": self.deepseek_model,
            "messages": [{"role": "user", "content": prompt}],
            "temperature": 0.3,
            "max_tokens": 4000
        }
        
        async with aiohttp.ClientSession() as session:
            try:
                async with session.post(url, json=payload, headers=headers, timeout=90) as response:
                    if response.status == 200:
                        result = await response.json()
                        claims_text = result['choices'][0]['message']['content']
                        print(f"   ✅ Extracted claims ({len(claims_text)} characters)")
                        return claims_text
                    else:
                        error_text = await response.text()
                        print(f"   ❌ DeepSeek error {response.status}: {error_text[:200]}")
                        return None
            except Exception as e:
                print(f"   ❌ Error: {type(e).__name__}: {str(e)[:100]}")
                return None
    
    async def find_supporting_evidence(self, claim: str, category: str):
        """Search all summaries for evidence supporting a specific claim"""
        print(f"\n   🔎 Searching for evidence: '{claim[:60]}...'")
        
        # Build a prompt that searches summaries
        summaries_text = ""
        for i, summary in enumerate(self.summaries[:20]):  # First 20 to stay under token limit
            summaries_text += f"\n\n--- SOURCE {i+1}: {summary['file']} ({summary['source']}) ---\n"
            summaries_text += summary['content'][:3000]  # First 3000 chars of each
        
        prompt = f"""You are analyzing research summaries to find evidence supporting a specific health claim.

**CLAIM TO VERIFY:**
"{claim}"

**CATEGORY:** {category}

**IMPORTANT CONTEXT:**
This claim is from a book about natural, whole-food, affordable health. The book recognizes that natural foods often lack research funding because they cannot be patented. Therefore, absence of peer-reviewed evidence does NOT mean a claim is invalid - it may simply reflect a research funding gap.

**YOUR TASK:**
Review the research summaries below and provide a nuanced assessment:

1. **Scientific Evidence** (PubMed articles, clinical studies):
   - Source filename
   - Relevant evidence (quote or paraphrase)
   - Strength: Strong / Moderate / Weak / Conflicting

2. **Historical/Traditional Evidence** (Gutenberg books from 1800s-1900s medical practice):
   - Source filename
   - What historical doctors/practitioners said
   - Strength: Strong / Moderate / Weak

3. **Research Gap Analysis:**
   - If NO evidence found, state: "No evidence found. This may be due to [lack of profit motive / obvious common knowledge / recent research gap]"
   - Suggest: "Recommend searching for: [specific search terms]"

**RESEARCH SUMMARIES:**
{summaries_text}

**FINDINGS:**"""

        url = "https://api.deepseek.com/v1/chat/completions"
        headers = {
            "Authorization": f"Bearer {self.deepseek_api_key}",
            "Content-Type": "application/json"
        }
        
        payload = {
            "model": self.deepseek_model,
            "messages": [{"role": "user", "content": prompt}],
            "temperature": 0.2,
            "max_tokens": 2000
        }
        
        async with aiohttp.ClientSession() as session:
            try:
                async with session.post(url, json=payload, headers=headers, timeout=60) as response:
                    if response.status == 200:
                        result = await response.json()
                        evidence = result['choices'][0]['message']['content']
                        print(f"      ✅ Found evidence ({len(evidence)} characters)")
                        return evidence
                    else:
                        error_text = await response.text()
                        print(f"      ❌ DeepSeek error {response.status}: {error_text[:200]}")
                        return None
            except Exception as e:
                print(f"      ❌ Error: {type(e).__name__}: {str(e)[:100]}")
                return None
    
    async def analyze_chapter(self, chapter_file: Path, output_file: Path):
        """Full analysis: extract claims and find supporting evidence"""
        print(f"\n{'='*70}")
        print(f"📖 ANALYZING: {chapter_file.name}")
        print(f"{'='*70}")
        
        # Step 1: Extract claims
        claims_text = await self.extract_chapter_claims(chapter_file)
        if not claims_text:
            print("❌ Failed to extract claims")
            return
        
        # Step 2: Parse claims (simple parsing - look for numbered claims)
        claims = []
        current_claim = {}
        
        for line in claims_text.split('\n'):
            line = line.strip()
            if re.match(r'^\d+\.', line):  # New claim starts
                if current_claim:
                    claims.append(current_claim)
                current_claim = {'raw': line}
            elif line.startswith('**Claim:**'):
                current_claim['claim'] = line.replace('**Claim:**', '').strip()
            elif line.startswith('**Category:**'):
                current_claim['category'] = line.replace('**Category:**', '').strip()
            elif line.startswith('**Why it matters:**'):
                current_claim['why'] = line.replace('**Why it matters:**', '').strip()
        
        if current_claim:
            claims.append(current_claim)
        
        print(f"\n📊 Found {len(claims)} health claims to verify")
        
        # Step 3: Find evidence for each claim (process in batches to avoid overwhelming API)
        print(f"\n🔬 Searching for supporting evidence...")
        
        results = []
        batch_size = 5  # Process 5 claims at a time
        
        for batch_start in range(0, len(claims), batch_size):
            batch_end = min(batch_start + batch_size, len(claims))
            batch = claims[batch_start:batch_end]
            
            print(f"\n{'='*70}")
            print(f"Processing claims {batch_start+1}-{batch_end} of {len(claims)}")
            print(f"{'='*70}")
            
            for i, claim_obj in enumerate(batch, batch_start+1):
                print(f"\n--- CLAIM {i}/{len(claims)} ---")
                
                claim = claim_obj.get('claim', claim_obj.get('raw', ''))
                category = claim_obj.get('category', 'General')
                
                evidence = await self.find_supporting_evidence(claim, category)
                
                results.append({
                    'claim': claim,
                    'category': category,
                    'why': claim_obj.get('why', ''),
                    'evidence': evidence
                })
                
                # Brief pause between claims
                await asyncio.sleep(2)
            
            # Longer pause between batches
            if batch_end < len(claims):
                print(f"\n⏸️  Pausing 5 seconds before next batch...")
                await asyncio.sleep(5)
        
        # Step 4: Generate report
        print(f"\n📝 Generating citation report...")
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(f"# Citation Report: {chapter_file.name}\n\n")
            f.write(f"**Generated:** {Path.cwd()}\n")
            f.write(f"**Sources analyzed:** {len(self.summaries)} research summaries\n")
            f.write(f"**Claims extracted:** {len(claims)}\n")
            f.write(f"**Claims verified:** {len(results)}\n\n")
            f.write("---\n\n")
            
            for i, result in enumerate(results, 1):
                f.write(f"## Claim {i}: {result['claim']}\n\n")
                f.write(f"**Category:** {result['category']}\n\n")
                if result['why']:
                    f.write(f"**Why it matters:** {result['why']}\n\n")
                f.write(f"### Supporting Evidence:\n\n")
                f.write(result['evidence'] if result['evidence'] else "No evidence found.")
                f.write("\n\n---\n\n")
        
        print(f"\n✅ Report saved: {output_file}")
        print(f"\n{'='*70}")
        print(f"📊 SUMMARY")
        print(f"{'='*70}")
        print(f"Total claims in chapter: {len(claims)}")
        print(f"Claims verified: {len(results)}")
        print(f"Sources searched: {len(self.summaries)}")
        print(f"Report location: {output_file}")

async def main():
    import argparse
    parser = argparse.ArgumentParser(description='Find supporting evidence for chapter claims')
    parser.add_argument('--chapter', type=str, required=True, help='Path to chapter file')
    parser.add_argument('--output', type=str, default='citation_report.md', help='Output report file')
    
    args = parser.parse_args()
    
    detector = PatternDetector('config.json')
    
    chapter_path = Path(args.chapter)
    output_path = Path(args.output)
    
    if not chapter_path.exists():
        print(f"❌ Chapter file not found: {chapter_path}")
        return
    
    await detector.analyze_chapter(chapter_path, output_path)

if __name__ == '__main__':
    asyncio.run(main())
