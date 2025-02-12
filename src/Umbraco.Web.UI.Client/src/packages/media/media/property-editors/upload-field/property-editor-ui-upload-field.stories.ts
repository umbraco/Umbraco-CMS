import type { UmbPropertyEditorUIUploadFieldElement } from './property-editor-ui-upload-field.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-upload-field.element.js';

export default {
	title: 'Property Editor UIs/Upload Field',
	component: 'umb-property-editor-ui-upload-field',
	id: 'umb-property-editor-ui-upload-field',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIUploadFieldElement> = () =>
	html`<umb-property-editor-ui-upload-field></umb-property-editor-ui-upload-field>`;
AAAOverview.storyName = 'Overview';
