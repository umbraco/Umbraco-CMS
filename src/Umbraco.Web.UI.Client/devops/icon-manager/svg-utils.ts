import type { IconNode } from './types.js';

export function iconNodesToSvg(nodes: IconNode[]): string {
	const children = nodes
		.map(([tag, attrs]) => {
			const attrStr = Object.entries(attrs)
				.map(([k, v]) => `${k}="${v}"`)
				.join(' ');
			return `<${tag} ${attrStr}/>`;
		})
		.join('');

	return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round">${children}</svg>`;
}
