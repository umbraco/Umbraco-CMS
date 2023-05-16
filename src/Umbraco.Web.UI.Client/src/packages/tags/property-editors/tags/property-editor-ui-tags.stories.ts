import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUITagsElement } from './property-editor-ui-tags.element';
import './property-editor-ui-tags.element';

export default {
	title: 'Property Editor UIs/Tags',
	component: 'umb-property-editor-ui-tags',
	id: 'umb-property-editor-ui-tags',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITagsElement> = () =>
	html`<umb-property-editor-ui-tags></umb-property-editor-ui-tags>`;
AAAOverview.storyName = 'Overview';
