import type { UmbPropertyEditorUIMultipleTextStringElement } from './property-editor-ui-multiple-text-string.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-multiple-text-string.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Multiple Text String',
	component: 'umb-property-editor-ui-multiple-text-string',
	id: 'umb-property-editor-ui-multiple-text-string',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIMultipleTextStringElement> = () =>
	html`<umb-property-editor-ui-multiple-text-string></umb-property-editor-ui-multiple-text-string>`;
