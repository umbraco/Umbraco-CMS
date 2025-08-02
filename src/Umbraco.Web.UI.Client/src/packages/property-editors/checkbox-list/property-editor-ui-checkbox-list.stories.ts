import type { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './property-editor-ui-checkbox-list.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Checkbox List',
	component: 'umb-property-editor-ui-checkbox-list',
	id: 'umb-property-editor-ui-checkbox-list',
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

export const Docs: StoryFn<UmbPropertyEditorUICheckboxListElement> = () =>
	html`<umb-property-editor-ui-checkbox-list .config=${config}></umb-property-editor-ui-checkbox-list>`;
