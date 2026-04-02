import type { UmbPropertyEditorUITagsElement } from './property-editor-ui-tags.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tags.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Tags',
	component: 'umb-property-editor-ui-tags',
	id: 'umb-property-editor-ui-tags',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUITagsElement> = () =>
	html`<umb-property-editor-ui-tags></umb-property-editor-ui-tags>`;
