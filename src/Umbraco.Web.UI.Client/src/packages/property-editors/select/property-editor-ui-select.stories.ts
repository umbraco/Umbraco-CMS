import type { UmbPropertyEditorUISelectElement } from './property-editor-ui-select.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './property-editor-ui-select.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Select',
	component: 'umb-property-editor-ui-select',
	id: 'umb-property-editor-ui-select',
} as Meta;

const config = new UmbPropertyEditorConfigCollection([
	{
		alias: 'items',
		value: [
			{ name: 'Option 1', value: 'option1' },
			{ name: 'Option 2', value: 'option2' },
			{ name: 'Option 3', value: 'option3' },
		],
	},
]);

export const Docs: StoryFn<UmbPropertyEditorUISelectElement> = () =>
	html`<umb-property-editor-ui-select .config=${config}></umb-property-editor-ui-select>`;
