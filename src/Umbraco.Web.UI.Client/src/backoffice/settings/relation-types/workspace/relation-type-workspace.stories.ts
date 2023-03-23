import './relation-type-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { data } from '../../../../core/mocks/data/relation-type.data';

import type { UmbRelationTypeWorkspaceElement } from './relation-type-workspace.element';

export default {
	title: 'Workspaces/Relation Type',
	component: 'umb-relation-type-workspace',
	id: 'umb-relation-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbRelationTypeWorkspaceElement> = () =>
	html` <umb-relation-type-workspace id="${data[0].key}"></umb-relation-type-workspace>`;
AAAOverview.storyName = 'Overview';
