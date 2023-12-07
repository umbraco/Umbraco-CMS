import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIUploadFieldElement } from './property-editor-ui-upload-field.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-upload-field.element.js';

export default {
	title: 'Property Editor UIs/Upload Field',
	component: 'umb-property-editor-ui-upload-field',
	id: 'umb-property-editor-ui-upload-field',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIUploadFieldElement> = () =>
	html`<umb-property-editor-ui-upload-field></umb-property-editor-ui-upload-field>`;
AAAOverview.storyName = 'Overview';
