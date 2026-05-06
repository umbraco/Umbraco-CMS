import iconDictionary from './icon-dictionary.json';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

export default {
	title: 'Generic Components/Icon/Available Lucide Icons',
	id: 'available-lucide-icons',
} as Meta;

interface UnregisteredIcon {
	name: string;
	svgPath: string;
	inUse?: boolean;
}

type LucideIconsData = Record<string, unknown>;

interface IconsResult {
	icons: UnregisteredIcon[];
	stats: {
		total: number;
		registered: number;
		unregistered: number;
	};
}

/**
 * Get all unregistered Lucide icons
 * @returns Promise with icons and statistics
 */
async function getUnregisteredIcons(): Promise<IconsResult> {
	// Get all registered lucide icon file names (without .svg extension)
	const registeredFiles = new Set(
		iconDictionary.lucide
			.filter((icon) => icon.file) // Filter out entries without a file property
			.map((icon) => icon.file?.replace('.svg', '')),
	);

	try {
		// Fetch lucide icons from static directory
		const response = await fetch('/lucide-static/icon-nodes.json');
		const lucideIcons: LucideIconsData = await response.json();

		const icons: UnregisteredIcon[] = Object.keys(lucideIcons)
			.map((lucideName) => ({
				name: lucideName,
				svgPath: `/lucide-static/icons/${lucideName}.svg`,
				inUse: registeredFiles.has(lucideName),
			}))
			.sort((a, b) => a.name.localeCompare(b.name));

		return {
			icons,
			stats: {
				total: Object.keys(lucideIcons).length,
				registered: iconDictionary.lucide.filter((icon) => icon.file).length,
				unregistered: icons.filter((icon) => !icon.inUse).length,
			},
		};
	} catch (error) {
		console.error('Failed to load lucide icons:', error);
		return {
			icons: [],
			stats: { total: 0, registered: iconDictionary.lucide.filter((icon) => icon.file).length, unregistered: 0 },
		};
	}
}

export const Docs: StoryObj = {
	render: () => {
		// Create a promise-based approach
		const data = getUnregisteredIcons();

		// Return immediately with a loading state, then update
		const container = document.createElement('div');
		container.innerHTML = '<div style="margin: var(--uui-size-layout-2);"><p>Loading...</p></div>';

		data.then(({ icons, stats }) => {
			const grid = icons.map(
				(icon) => `
				<div style="
					display: flex;
					flex-direction: column;
					align-items: center;
					justify-content: center;
					text-align: center;
					width: 100%;
					height: 100%;
					padding: var(--uui-size-space-3);
					${
						icon.inUse
							? `border: 2px solid var(--uui-color-selected);`
							: `
						border: 1px solid var(--uui-color-border);
						`
					}
					border-radius: var(--uui-border-radius);
					background-color: var(--uui-color-surface);">
					<img
						src="${icon.svgPath}"
						alt="${icon.name}"
						width="18"
						height="18"
						style="margin-bottom: 9px;"
						loading="lazy" />
					<small style="word-break: break-word; opacity: 0.6;">${icon.name}</small>
				</div>
			`,
			);

			container.innerHTML = `
				<div style="margin: var(--uui-size-layout-2);">
					<h2>Available Lucide Icons</h2>
					<p>
						<strong>Total Lucide Icons:</strong> ${stats.total}<br />
						<strong>Registered in CMS:</strong> ${stats.registered} (Marked with blue)<br />
						<strong>Unregistered:</strong> ${stats.unregistered}
					</p>
					<p style="color: var(--uui-color-text-secondary);">
						These icons are available in Lucide but not yet registered in the Umbraco CMS icon registry.
						You can contribute new icon registrations by making a PR to the CMS source code. In the CMS Source Code add the desired icon by adding it to the 'icon-dictionary.json', under 'lucide'. Afterwards run 'npm run generate:icons'.
						Note: You can also add icons just in your own project. For more information, see the <a href="https://docs.umbraco.com/umbraco-cms/customizing/extending-overview/extension-types/icons" target="_blank" rel="noopener noreferrer">Umbraco Icons Extension documentation</a>.
					</p>
				</div>
				<div style="display: grid;
					grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
					grid-gap: var(--uui-size-layout-2);
					margin: var(--uui-size-layout-2);
					place-items: start;
					justify-content: space-between;">
					${grid.join('')}
				</div>
			`;
		});

		return container;
	},
};
