import type { UmbPropertyEditorUIAcceptedUploadTypesElement } from './property-editor-ui-accepted-upload-types.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-accepted-upload-types.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Accepted Upload Types',
	component: 'umb-property-editor-ui-accepted-upload-types',
	id: 'umb-property-editor-ui-accepted-upload-types',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIAcceptedUploadTypesElement> = () =>
	html`<umb-property-editor-ui-accepted-upload-types></umb-property-editor-ui-accepted-upload-types>`;
