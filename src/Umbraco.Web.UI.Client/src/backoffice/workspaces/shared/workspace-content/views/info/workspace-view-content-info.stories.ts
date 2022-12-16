import './workspace-view-content-info.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../../core/mocks/data/document.data';
import { UmbNodeContext } from '../../node.context';

import type { UmbWorkspaceViewContentInfoElement } from './workspace-view-content-info.element';

export default {
	title: 'Workspaces/Shared/Node/Views/Info',
	component: 'umb-workspace-view-content-info',
	id: 'umb-workspace-view-content-info',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbNodeContext" .value=${new UmbNodeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewContentInfoElement> = () =>
	html` <umb-workspace-view-content-info></umb-workspace-view-content-info>`;
AAAOverview.storyName = 'Overview';
