import type { UmbPropertyEditorUIValueTypeElement } from './property-editor-ui-value-type.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-value-type.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Value Type',
	component: 'umb-property-editor-ui-value-type',
	id: 'umb-property-editor-ui-value-type',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIValueTypeElement> = () =>
	html`<umb-property-editor-ui-value-type></umb-property-editor-ui-value-type>`;
