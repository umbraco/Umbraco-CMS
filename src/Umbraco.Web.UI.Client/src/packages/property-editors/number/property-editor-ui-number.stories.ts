import type { UmbPropertyEditorUINumberElement } from './property-editor-ui-number.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-number.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Number',
	component: 'umb-property-editor-ui-number',
	id: 'umb-property-editor-ui-number',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUINumberElement> = () =>
	html` <umb-property-editor-ui-number></umb-property-editor-ui-number>`;
