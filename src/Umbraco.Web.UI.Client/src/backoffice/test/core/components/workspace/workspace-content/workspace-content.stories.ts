import './workspace-content.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../../core/mocks/data/document.data';

import type { UmbWorkspaceContentElement } from './workspace-content.element';

export default {
	title: 'Workspaces/Shared/Node',
	component: 'umb-workspace-content',
	id: 'umb-workspace-content',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceContentElement> = () =>
	html` <umb-workspace-content id="${data[0].key}"></umb-workspace-content>`;
AAAOverview.storyName = 'Overview';
