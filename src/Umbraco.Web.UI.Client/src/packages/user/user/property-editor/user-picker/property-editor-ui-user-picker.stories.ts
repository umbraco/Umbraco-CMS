import type { UmbPropertyEditorUIUserPickerElement } from './property-editor-ui-user-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-user-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/User Picker',
	component: 'umb-property-editor-ui-user-picker',
	id: 'umb-property-editor-ui-user-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIUserPickerElement> = () =>
	html`<umb-property-editor-ui-user-picker></umb-property-editor-ui-user-picker>`;
