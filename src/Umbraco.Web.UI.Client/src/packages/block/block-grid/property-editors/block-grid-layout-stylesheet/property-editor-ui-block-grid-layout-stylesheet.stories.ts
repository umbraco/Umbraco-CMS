import type { UmbPropertyEditorUIBlockGridLayoutStylesheetElement } from './property-editor-ui-block-grid-layout-stylesheet.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-layout-stylesheet.element.js';

export default {
	title: 'Property Editor UIs/Block Grid Layout Stylesheet',
	component: 'umb-property-editor-ui-block-grid-layout-stylesheet',
	id: 'umb-property-editor-ui-block-grid-layout-stylesheet',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockGridLayoutStylesheetElement> = () =>
	html` <umb-property-editor-ui-block-grid-layout-stylesheet></umb-property-editor-ui-block-grid-layout-stylesheet>`;
AAAOverview.storyName = 'Overview';
