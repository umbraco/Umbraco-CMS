import type { UmbPropertyEditorUISliderElement } from './property-editor-ui-slider.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './property-editor-ui-slider.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Slider',
	component: 'umb-property-editor-ui-slider',
	id: 'umb-property-editor-ui-slider',
} as Meta;

const config = new UmbPropertyEditorConfigCollection([
	{
		alias: 'maxVal',
		value: 100,
	},
	{
		alias: 'step',
		value: 10,
	},
]);

export const Docs: StoryFn<UmbPropertyEditorUISliderElement> = () =>
	html`<umb-property-editor-ui-slider .config="${config}"></umb-property-editor-ui-slider>`;
