import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbEditorNodeElement } from './editor-node.element';
import './editor-node.element';

import { data } from '../../../../mocks/data/node.data';

export default {
	title: 'Editors/Shared/Node',
	component: 'umb-editor-node',
	id: 'umb-editor-node',
} as Meta;

export const AAAOverview: Story<UmbEditorNodeElement> = () =>
	html` <umb-editor-node id="${data[0].id}"></umb-editor-node>`;
AAAOverview.storyName = 'Overview';
