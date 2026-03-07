/**
 * Returns true when every cell in a row is a `<th>`.
 * @param {Element} row - A `<tr>` element to inspect.
 * @returns {boolean} Whether the row is a header row.
 */
function isHeaderRow(row: Element): boolean {
	const cells = row.querySelectorAll(':scope > td, :scope > th');
	return cells.length > 0 && Array.from(cells).every((cell) => cell.tagName === 'TH');
}

/**
 * Collects leading consecutive header rows from a `<tbody>`.
 * Stops at the first row that contains any `<td>` cell.
 * @param {HTMLTableSectionElement} tbody - The table body to scan.
 * @returns {Array<Element>} The leading rows whose cells are all `<th>`.
 */
function collectLeadingHeaderRows(tbody: HTMLTableSectionElement): Element[] {
	const rows = Array.from(tbody.querySelectorAll(':scope > tr'));
	const headerRows: Element[] = [];

	for (const row of rows) {
		if (!isHeaderRow(row)) break;
		headerRows.push(row);
	}
	return headerRows;
}

/**
 * Moves leading `<th>`-only rows from `<tbody>` into a new `<thead>` on a single table.
 * @param {HTMLTableElement} table - The table element to process.
 * @returns {boolean} Whether the table was modified.
 */
function wrapHeaderRows(table: HTMLTableElement): boolean {
	const tbody = table.querySelector(':scope > tbody') as HTMLTableSectionElement | null;
	if (!tbody || table.querySelector(':scope > thead')) return false;

	const headerRows = collectLeadingHeaderRows(tbody);
	if (headerRows.length === 0) return false;

	const thead = table.ownerDocument.createElement('thead');
	headerRows.forEach((row) => thead.appendChild(row));
	table.insertBefore(thead, tbody);
	return true;
}

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
	if (!html.includes('<table')) return html;

	const doc = new DOMParser().parseFromString(`<body>${html}</body>`, 'text/html');
	const tables = doc.querySelectorAll('table');

	if (tables.length === 0) return html;

	let modified = false;
	tables.forEach((table) => {
		modified = wrapHeaderRows(table) || modified;
	});

	return modified ? doc.body.innerHTML : html;
}
