import type { UmbIconPickerModalElement } from '@umbraco-cms/backoffice/icon';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-icon-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Icon Picker',
	component: 'umb-property-editor-ui-icon-picker',
	id: 'umb-property-editor-ui-icon-picker',
} as Meta;

export const Docs: StoryFn<UmbIconPickerModalElement> = () =>
	html`<umb-property-editor-ui-icon-picker></umb-property-editor-ui-icon-picker>`;
