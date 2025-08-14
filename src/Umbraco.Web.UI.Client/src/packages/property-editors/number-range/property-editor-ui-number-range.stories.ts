import type { UmbPropertyEditorUINumberRangeElement } from './property-editor-ui-number-range.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-number-range.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Number Range',
	component: 'umb-property-editor-ui-number-range',
	id: 'umb-property-editor-ui-number-range',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUINumberRangeElement> = () =>
	html`<umb-property-editor-ui-number-range></umb-property-editor-ui-number-range>`;
