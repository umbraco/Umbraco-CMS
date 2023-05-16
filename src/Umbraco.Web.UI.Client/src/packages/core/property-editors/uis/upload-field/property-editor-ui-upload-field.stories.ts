import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIUploadFieldElement } from './property-editor-ui-upload-field.element';
import './property-editor-ui-upload-field.element';

export default {
	title: 'Property Editor UIs/Upload Field',
	component: 'umb-property-editor-ui-upload-field',
	id: 'umb-property-editor-ui-upload-field',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIUploadFieldElement> = () =>
	html`<umb-property-editor-ui-upload-field></umb-property-editor-ui-upload-field>`;
AAAOverview.storyName = 'Overview';
