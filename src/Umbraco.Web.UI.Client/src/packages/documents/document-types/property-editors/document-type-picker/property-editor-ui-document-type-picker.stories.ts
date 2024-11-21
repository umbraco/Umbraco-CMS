import type { UmbPropertyEditorUIDocumentTypePickerElement } from './property-editor-ui-document-type-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-document-type-picker.element.js';

export default {
	title: 'Property Editor UIs/Document Type Picker',
	component: 'umb-property-editor-ui-document-type-picker',
	id: 'umb-property-editor-ui-document-type-picker',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIDocumentTypePickerElement> = () =>
	html` <umb-property-editor-ui-document-type-picker></umb-property-editor-ui-document-type-picker>`;
AAAOverview.storyName = 'Overview';
