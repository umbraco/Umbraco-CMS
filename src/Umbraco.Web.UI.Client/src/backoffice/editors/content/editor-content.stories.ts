import './editor-content.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../mocks/data/node.data';

import type { UmbEditorContentElement } from './editor-content.element';

export default {
	title: 'Editors/Content',
	component: 'umb-editor-content',
	id: 'umb-editor-content',
} as Meta;

const documentNodes = data.filter((node) => node.type === 'document');

export const AAAOverview: Story<UmbEditorContentElement> = () =>
	html` <umb-editor-content id="${documentNodes[0].id}"></umb-editor-content>`;
AAAOverview.storyName = 'Overview';
