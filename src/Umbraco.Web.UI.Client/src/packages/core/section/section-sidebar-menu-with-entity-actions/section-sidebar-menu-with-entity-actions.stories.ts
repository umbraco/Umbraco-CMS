import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbSectionSidebarMenuWithEntityActionsElement } from './section-sidebar-menu-with-entity-actions.element.js';
import './section-sidebar-menu-with-entity-actions.element.js';

export default {
	title: 'Sections/Shared/Section Sidebar Menu With Entity Actions',
	component: 'umb-section-sidebar-menu-with-entity-actions',
	id: 'umb-section-sidebar-menu-with-entity-actions',
} as Meta;

export const AAAOverview: Story<UmbSectionSidebarMenuWithEntityActionsElement> = () =>
	html` <umb-section-sidebar-menu-with-entity-actions></umb-section-sidebar-menu-with-entity-actions>`;
AAAOverview.storyName = 'Overview';
