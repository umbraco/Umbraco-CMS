import './member-group-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { data } from '../../../../core/mocks/data/member-group.data';

import type { UmbMemberGroupWorkspaceElement } from './member-group-workspace.element';

export default {
	title: 'Workspaces/Member Group',
	component: 'umb-member-group-workspace',
	id: 'umb-member-group-workspace',
} as Meta;

export const AAAOverview: Story<UmbMemberGroupWorkspaceElement> = () =>
	html` <umb-member-group-workspace id="${data[0].key}"></umb-member-group-workspace>`;
AAAOverview.storyName = 'Overview';
