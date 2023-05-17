import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUITinyMceElement } from './property-editor-ui-tiny-mce.element';
import './property-editor-ui-tiny-mce.element';

export default {
	title: 'Property Editor UIs/Tiny Mce',
	component: 'umb-property-editor-ui-tiny-mce',
	id: 'umb-property-editor-ui-tiny-mce',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceElement> = () =>
	html`<umb-property-editor-ui-tiny-mce></umb-property-editor-ui-tiny-mce>`;
AAAOverview.storyName = 'Overview';
