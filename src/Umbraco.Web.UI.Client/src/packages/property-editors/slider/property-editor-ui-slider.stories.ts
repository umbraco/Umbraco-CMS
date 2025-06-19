import type { UmbPropertyEditorUISliderElement } from './property-editor-ui-slider.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-slider.element.js';

export default {
	title: 'Property Editor UIs/Slider',
	component: 'umb-property-editor-ui-slider',
	id: 'umb-property-editor-ui-slider',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUISliderElement> = () =>
	html`<umb-property-editor-ui-slider
		.config="${[
			{ alias: 'maxVal', value: 50 },
			{ alias: 'step', value: 5 },
		]}"></umb-property-editor-ui-slider>`;
AAAOverview.storyName = 'Overview';
