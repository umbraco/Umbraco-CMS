import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUITagsStorageTypeElement } from './property-editor-ui-tags-storage-type.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tags-storage-type.element.js';

export default {
	title: 'Property Editor UIs/Tags Storage Type',
	component: 'umb-property-editor-ui-tags-storage-type',
	id: 'umb-property-editor-ui-tags-storage-type',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITagsStorageTypeElement> = () =>
	html`<umb-property-editor-ui-tags-storage-type></umb-property-editor-ui-tags-storage-type>`;
AAAOverview.storyName = 'Overview';
