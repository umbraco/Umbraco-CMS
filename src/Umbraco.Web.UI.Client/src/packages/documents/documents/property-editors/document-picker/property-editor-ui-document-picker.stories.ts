import type { UmbPropertyEditorUIDocumentPickerElement } from './property-editor-ui-document-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-document-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Document Picker',
	component: 'umb-property-editor-ui-document-picker',
	id: 'umb-property-editor-ui-document-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIDocumentPickerElement> = () =>
	html` <umb-property-editor-ui-document-picker></umb-property-editor-ui-document-picker>`;
