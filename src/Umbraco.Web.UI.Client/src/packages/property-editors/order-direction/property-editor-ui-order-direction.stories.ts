import type { UmbPropertyEditorUIOrderDirectionElement } from './property-editor-ui-order-direction.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-order-direction.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Order Direction',
	component: 'umb-property-editor-ui-order-direction',
	id: 'umb-property-editor-ui-order-direction',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIOrderDirectionElement> = () =>
	html`<umb-property-editor-ui-order-direction></umb-property-editor-ui-order-direction>`;
