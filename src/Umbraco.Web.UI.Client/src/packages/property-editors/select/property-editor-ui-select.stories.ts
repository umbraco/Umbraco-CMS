import type { UmbPropertyEditorUISelectElement } from './property-editor-ui-select.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-select.element.js';

export default {
	title: 'Property Editor UIs/Select',
	component: 'umb-property-editor-ui-select',
	id: 'umb-property-editor-ui-select',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUISelectElement> = () =>
	html`<umb-property-editor-ui-select></umb-property-editor-ui-select>`;
AAAOverview.storyName = 'Overview';
