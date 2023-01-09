import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUISliderElement } from './property-editor-ui-slider.element';
import './property-editor-ui-slider.element';

export default {
	title: 'Property Editor UIs/Slider',
	component: 'umb-property-editor-ui-slider',
	id: 'umb-property-editor-ui-slider',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUISliderElement> = () =>
	html`<umb-property-editor-ui-slider></umb-property-editor-ui-slider>`;
AAAOverview.storyName = 'Overview';
