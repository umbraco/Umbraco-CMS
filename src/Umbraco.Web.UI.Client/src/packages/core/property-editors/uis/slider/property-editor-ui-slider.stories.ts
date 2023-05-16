import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUISliderElement } from './property-editor-ui-slider.element';
import './property-editor-ui-slider.element';

export default {
	title: 'Property Editor UIs/Slider',
	component: 'umb-property-editor-ui-slider',
	id: 'umb-property-editor-ui-slider',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUISliderElement> = () =>
	html`<umb-property-editor-ui-slider
		.config="${[
			{ alias: 'maxVal', value: 50 },
			{ alias: 'step', value: 5 },
		]}"></umb-property-editor-ui-slider>`;
AAAOverview.storyName = 'Overview';
