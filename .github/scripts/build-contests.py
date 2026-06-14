"""
Fetches the DXLog.net supported contest list and extracts
Contest Name + Cabrillo Name pairs into docs/data/contests.json.

Run during the GitHub Pages build (see .github/workflows/pages.yml).
"""

import json
import os
import urllib.request
from html.parser import HTMLParser

URL = "https://dxlog.net/sw/contestlist.php"
OUTPUT = os.path.join("docs", "data", "contests.json")


class ContestTableParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.in_table = False
        self.in_row = False
        self.in_cell = False
        self.current_row = []
        self.current_cell = []
        self.rows = []

    def handle_starttag(self, tag, attrs):
        if tag == "table":
            self.in_table = True
        elif tag == "tr" and self.in_table:
            self.in_row = True
            self.current_row = []
        elif tag in ("td", "th") and self.in_row:
            self.in_cell = True
            self.current_cell = []

    def handle_endtag(self, tag):
        if tag == "table":
            self.in_table = False
        elif tag == "tr" and self.in_row:
            self.in_row = False
            self.rows.append(self.current_row)
        elif tag in ("td", "th") and self.in_cell:
            self.in_cell = False
            self.current_row.append("".join(self.current_cell).strip())

    def handle_data(self, data):
        if self.in_cell:
            self.current_cell.append(data)


def main():
    req = urllib.request.Request(URL, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=30) as resp:
        html = resp.read().decode("utf-8", errors="replace")

    parser = ContestTableParser()
    parser.feed(html)

    contests = []
    seen = set()
    for row in parser.rows:
        if len(row) < 3:
            continue
        name, _group, cabrillo = row[0], row[1], row[2]
        if not name or name.lower() == "contest name":
            continue
        key = (name, cabrillo)
        if key in seen:
            continue
        seen.add(key)
        contests.append({"name": name, "cabrillo": cabrillo})

    contests.sort(key=lambda c: c["name"].lower())

    os.makedirs(os.path.dirname(OUTPUT), exist_ok=True)
    with open(OUTPUT, "w", encoding="utf-8") as f:
        json.dump(contests, f, ensure_ascii=False, indent=2)

    print(f"Wrote {len(contests)} contests to {OUTPUT}")


if __name__ == "__main__":
    main()