import type { UmbPropertyEditorUIContentPickerElement } from './property-editor-ui-content-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-content-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Content Picker/Content Picker',
	component: 'umb-property-editor-ui-content-picker',
	id: 'umb-property-editor-ui-content-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIContentPickerElement> = () =>
	html`<umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker>`;
