import type { UmbPropertyEditorUITextBoxElement } from './property-editor-ui-text-box.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-text-box.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Text Box',
	component: 'umb-property-editor-ui-text-box',
	id: 'umb-property-editor-ui-text-box',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUITextBoxElement> = () =>
	html` <umb-property-editor-ui-text-box></umb-property-editor-ui-text-box>`;
