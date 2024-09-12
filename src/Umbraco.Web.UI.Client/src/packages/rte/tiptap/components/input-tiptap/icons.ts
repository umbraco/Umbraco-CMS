import { html } from '@umbraco-cms/backoffice/external/lit';

const iconSize = '16px';
export const bold = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-bold">
	<path d="M6 12h9a4 4 0 0 1 0 8H7a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1h7a4 4 0 0 1 0 8" />
</svg>`;

export const italic = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-italic">
	<line x1="19" x2="10" y1="4" y2="4" />
	<line x1="14" x2="5" y1="20" y2="20" />
	<line x1="15" x2="9" y1="4" y2="20" />
</svg>`;

export const underline = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-underline">
	<path d="M6 4v6a6 6 0 0 0 12 0V4" />
	<line x1="4" x2="20" y1="20" y2="20" />
</svg>`;

export const strikethrough = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-strikethrough">
	<path d="M16 4H9a3 3 0 0 0-2.83 4" />
	<path d="M14 12a4 4 0 0 1 0 8H6" />
	<line x1="4" x2="20" y1="12" y2="12" />
</svg>`;
export const heading1 = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-heading-1">
	<path d="M4 12h8" />
	<path d="M4 18V6" />
	<path d="M12 18V6" />
	<path d="m17 12 3-2v8" />
</svg>`;
export const heading2 = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-heading-2">
	<path d="M4 12h8" />
	<path d="M4 18V6" />
	<path d="M12 18V6" />
	<path d="M21 18h-4c0-4 4-3 4-6 0-1.5-2-2.5-4-1" />
</svg>`;
export const heading3 = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-heading-3">
	<path d="M4 12h8" />
	<path d="M4 18V6" />
	<path d="M12 18V6" />
	<path d="M17.5 10.5c1.7-1 3.5 0 3.5 1.5a2 2 0 0 1-2 2" />
	<path d="M17 17.5c2 1.5 4 .3 4-1.5a2 2 0 0 0-2-2" />
</svg>`;
export const blockquote = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-text-quote">
	<path d="M17 6H3" />
	<path d="M21 12H8" />
	<path d="M21 18H8" />
	<path d="M3 12v6" />
</svg>`;
export const code = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-code">
	<polyline points="16 18 22 12 16 6" />
	<polyline points="8 6 2 12 8 18" />
</svg>`;
export const bulletList = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-list">
	<line x1="8" x2="21" y1="6" y2="6" />
	<line x1="8" x2="21" y1="12" y2="12" />
	<line x1="8" x2="21" y1="18" y2="18" />
	<line x1="3" x2="3.01" y1="6" y2="6" />
	<line x1="3" x2="3.01" y1="12" y2="12" />
	<line x1="3" x2="3.01" y1="18" y2="18" />
</svg>`;
export const orderedList = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-list-ordered">
	<line x1="10" x2="21" y1="6" y2="6" />
	<line x1="10" x2="21" y1="12" y2="12" />
	<line x1="10" x2="21" y1="18" y2="18" />
	<path d="M4 6h1v4" />
	<path d="M4 10h2" />
	<path d="M6 18H4c0-1 2-2 2-3s-1-1.5-2-1" />
</svg>`;
export const horizontalRule = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-separator-horizontal">
	<line x1="3" x2="21" y1="12" y2="12" />
	<polyline points="8 8 12 4 16 8" />
	<polyline points="16 16 12 20 8 16" />
</svg>`;
export const alignLeft = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-align-left">
	<line x1="21" x2="3" y1="6" y2="6" />
	<line x1="15" x2="3" y1="12" y2="12" />
	<line x1="17" x2="3" y1="18" y2="18" />
</svg>`;
export const alignCenter = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-align-center">
	<line x1="21" x2="3" y1="6" y2="6" />
	<line x1="17" x2="7" y1="12" y2="12" />
	<line x1="19" x2="5" y1="18" y2="18" />
</svg>`;
export const alignRight = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-align-right">
	<line x1="21" x2="3" y1="6" y2="6" />
	<line x1="21" x2="9" y1="12" y2="12" />
	<line x1="21" x2="7" y1="18" y2="18" />
</svg>`;
export const alignJustify = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-align-justify">
	<line x1="3" x2="21" y1="6" y2="6" />
	<line x1="3" x2="21" y1="12" y2="12" />
	<line x1="3" x2="21" y1="18" y2="18" />
</svg>`;

export const link = html`<svg
	xmlns="http://www.w3.org/2000/svg"
	width=${iconSize}
	height=${iconSize}
	viewBox="0 0 24 24"
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	class="lucide lucide-link">
	<path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71" />
	<path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71" />
</svg>`;
