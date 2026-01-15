import type { UmbPropertyEditorUIUploadFieldElement } from './property-editor-ui-upload-field.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-upload-field.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Upload Field',
	component: 'umb-property-editor-ui-upload-field',
	id: 'umb-property-editor-ui-upload-field',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIUploadFieldElement> = () =>
	html`<umb-property-editor-ui-upload-field></umb-property-editor-ui-upload-field>`;
