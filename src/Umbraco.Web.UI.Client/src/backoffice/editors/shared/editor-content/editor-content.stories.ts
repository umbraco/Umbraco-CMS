import './editor-content.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../core/mocks/data/document.data';

import type { UmbEditorContentElement } from './editor-content.element';

export default {
	title: 'Editors/Shared/Node',
	component: 'umb-editor-content',
	id: 'umb-editor-content',
} as Meta;

export const AAAOverview: Story<UmbEditorContentElement> = () =>
	html` <umb-editor-content id="${data[0].key}"></umb-editor-content>`;
AAAOverview.storyName = 'Overview';
