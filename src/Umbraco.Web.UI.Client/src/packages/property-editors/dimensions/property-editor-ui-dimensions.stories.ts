import type { UmbPropertyEditorUIDimensionsElement } from './property-editor-ui-dimensions.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-dimensions.element.js';

export default {
	title: 'Property Editor UIs/Dimensions',
	component: 'umb-property-editor-ui-dimensions',
	id: 'umb-property-editor-ui-dimensions',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIDimensionsElement> = () =>
	html` <umb-property-editor-ui-dimensions></umb-property-editor-ui-dimensions>`;
AAAOverview.storyName = 'Overview';
