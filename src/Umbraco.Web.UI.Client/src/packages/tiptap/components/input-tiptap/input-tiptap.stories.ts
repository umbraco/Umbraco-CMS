import type { UmbInputTiptapElement } from './input-tiptap.element.js';
import { manifests as tiptapManifests } from '../../manifests.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

import './input-tiptap.element.js';

// Standalone use case: a custom dashboard, workspace view, or external package can
// import `<umb-input-tiptap>` directly. The story exists primarily so we can
// visually confirm in Storybook that registering the package manifests and
// rendering the element on its own — without `<umb-property-editor-ui-tiptap>` —
// still triggers the lazy `extension-apis.bundle` chunk and instantiates the
// Tiptap editor end-to-end.

// One-time manifest registration (Storybook keeps the registry alive across stories).
let manifestsRegistered = false;
function ensureManifestsRegistered() {
	if (manifestsRegistered) return;
	umbExtensionsRegistry.registerMany(tiptapManifests);
	manifestsRegistered = true;
}

const minimalConfig = new UmbPropertyEditorConfigCollection([
	{ alias: 'dimensions', value: { height: 300 } },
	{ alias: 'extensions', value: ['Umb.Tiptap.RichTextEssentials'] },
]);

const richConfig = new UmbPropertyEditorConfigCollection([
	{ alias: 'dimensions', value: { height: 400 } },
	{
		alias: 'extensions',
		value: [
			'Umb.Tiptap.RichTextEssentials',
			'Umb.Tiptap.Bold',
			'Umb.Tiptap.Italic',
			'Umb.Tiptap.Underline',
			'Umb.Tiptap.BulletList',
			'Umb.Tiptap.OrderedList',
		],
	},
	{
		alias: 'toolbar',
		value: [
			[
				['Umb.Tiptap.Toolbar.Bold', 'Umb.Tiptap.Toolbar.Italic', 'Umb.Tiptap.Toolbar.Underline'],
				['Umb.Tiptap.Toolbar.BulletList', 'Umb.Tiptap.Toolbar.OrderedList'],
			],
		],
	},
]);

const meta: Meta<UmbInputTiptapElement> = {
	title: 'Components/Inputs/Tiptap (standalone)',
	component: 'umb-input-tiptap',
	id: 'umb-input-tiptap',
	decorators: [
		(story) => {
			ensureManifestsRegistered();
			return story();
		},
	],
};

export default meta;
type Story = StoryObj<UmbInputTiptapElement>;

export const EssentialsOnly: Story = {
	render: () => html`
		<umb-input-tiptap
			.configuration=${minimalConfig}
			.value=${'<p>Standalone <umb-input-tiptap> with only Rich Text Essentials.</p>'}>
		</umb-input-tiptap>
	`,
};

export const WithFormattingToolbar: Story = {
	render: () => html`
		<umb-input-tiptap
			.configuration=${richConfig}
			.value=${'<p>Standalone <umb-input-tiptap> with a minimal toolbar.</p>'}>
		</umb-input-tiptap>
	`,
};
