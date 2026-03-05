/**
 * Ensures that table HTML has proper `<thead>` and `<tbody>` sections for WCAG 2.2 compliance.
 * ProseMirror's table schema stores all rows in a flat structure without `<thead>`/`<tbody>` section
 * grouping. When serializing to HTML via `getHTML()`, all rows are placed in a single `<tbody>`.
 * This function post-processes the HTML to detect leading rows with all `<th>` cells and wraps
 * them in a `<thead>` section, ensuring correct semantic structure for assistive technologies.
 * This addresses WCAG 2.2 SC 1.3.1 (Info and Relationships) and SC 4.1.2 (Name, Role, Value)
 * by ensuring the programmatic relationship between header cells and data cells can be
 * determined by assistive technologies.
 * @param {string} html - The HTML string output from TipTap's `editor.getHTML()`.
 * @returns {string} The HTML string with proper `<thead>` and `<tbody>` table sections.
 */
export function ensureTableSections(html: string): string {
	// Quick check — if no table elements exist, return unchanged.
	if (!html.includes('<table')) return html;

	const parser = new DOMParser();
	const doc = parser.parseFromString(`<body>${html}</body>`, 'text/html');
	const tables = doc.querySelectorAll('table');

	if (tables.length === 0) return html;

	let modified = false;

	tables.forEach((table) => {
		// ProseMirror always renders rows inside a <tbody>.
		const tbody = table.querySelector(':scope > tbody');
		if (!tbody) return;

		// If a <thead> already exists, nothing to do for this table.
		if (table.querySelector(':scope > thead')) return;

		const rows = Array.from(tbody.querySelectorAll(':scope > tr'));
		const headerRows: Element[] = [];

		// Collect leading consecutive rows where every direct cell child is a <th>.
		for (const row of rows) {
			const cells = row.querySelectorAll(':scope > td, :scope > th');
			if (cells.length === 0) break;

			const allTh = Array.from(cells).every((cell) => cell.tagName === 'TH');
			if (allTh) {
				headerRows.push(row);
			} else {
				break; // Stop at the first non-header row.
			}
		}

		if (headerRows.length > 0) {
			const thead = doc.createElement('thead');
			headerRows.forEach((row) => thead.appendChild(row));
			table.insertBefore(thead, tbody);
			modified = true;
		}
	});

	if (!modified) return html;

	return doc.body.innerHTML;
}
