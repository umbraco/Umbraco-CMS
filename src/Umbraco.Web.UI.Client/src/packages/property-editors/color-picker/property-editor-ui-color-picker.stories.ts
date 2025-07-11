import type { UmbPropertyEditorUIColorPickerElement } from './property-editor-ui-color-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-color-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Color Picker',
	component: 'umb-property-editor-ui-color-picker',
	id: 'umb-property-editor-ui-color-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIColorPickerElement> = () =>
	html`<umb-property-editor-ui-color-picker></umb-property-editor-ui-color-picker>`;
