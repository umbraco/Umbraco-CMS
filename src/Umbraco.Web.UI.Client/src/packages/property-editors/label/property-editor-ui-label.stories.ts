import type { UmbPropertyEditorUILabelElement } from './property-editor-ui-label.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-label.element.js';

export default {
	title: 'Property Editor UIs/Label',
	component: 'umb-property-editor-ui-label',
	id: 'umb-property-editor-ui-label',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUILabelElement> = () =>
	html`<umb-property-editor-ui-label></umb-property-editor-ui-label>`;
AAAOverview.storyName = 'Overview';
