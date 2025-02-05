import type { UmbPropertyEditorUIAcceptedUploadTypesElement } from './property-editor-ui-accepted-upload-types.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-accepted-types.element.js';

export default {
	title: 'Property Editor UIs/Accepted Types',
	component: 'umb-property-editor-ui-accepted-types',
	id: 'umb-property-editor-ui-accepted-types',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIAcceptedUploadTypesElement> = () =>
	html`<umb-property-editor-ui-accepted-upload-types></umb-property-editor-ui-accepted-upload-types>`;
AAAOverview.storyName = 'Overview';
