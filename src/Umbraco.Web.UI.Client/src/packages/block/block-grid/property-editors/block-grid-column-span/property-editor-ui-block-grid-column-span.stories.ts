import type { UmbPropertyEditorUIBlockGridColumnSpanElement } from './property-editor-ui-block-grid-column-span.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-column-span.element.js';

export default {
	title: 'Property Editor UIs/Block Grid Column Span',
	component: 'umb-property-editor-ui-block-grid-column-span',
	id: 'umb-property-editor-ui-block-grid-column-span',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockGridColumnSpanElement> = () =>
	html` <umb-property-editor-ui-block-grid-column-span></umb-property-editor-ui-block-grid-column-span>`;
AAAOverview.storyName = 'Overview';
