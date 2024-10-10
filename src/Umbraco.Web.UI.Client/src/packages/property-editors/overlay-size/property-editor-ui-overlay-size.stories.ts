import type { UmbPropertyEditorUIOverlaySizeElement } from './property-editor-ui-overlay-size.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-overlay-size.element.js';

export default {
	title: 'Property Editor UIs/Overlay Size',
	component: 'umb-property-editor-ui-overlay-size',
	id: 'umb-property-editor-ui-overlay-size',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIOverlaySizeElement> = () =>
	html`<umb-property-editor-ui-overlay-size></umb-property-editor-ui-overlay-size>`;
AAAOverview.storyName = 'Overview';
