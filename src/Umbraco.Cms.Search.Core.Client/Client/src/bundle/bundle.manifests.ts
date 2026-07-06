import { manifests as localizationManifests } from './lang/manifests.js';
import { manifests as collectionActionManifests } from './collectionActions.manifests.js';
import { manifests as collectionViewManifests } from './collectionViews.manifests.js';
import { manifests as conditionManifests } from './condition.manifests.js';
import { manifests as entityActionManifests } from './entityActions.manifests.js';
import { manifests as globalContextManifests } from './globalContexts.manifests.js';
import { manifests as repositoryManifests } from './repositories.manifests.js';
import { manifests as searchRootWorkspaceManifests } from './search-root-workspace.manifests.js';
import { manifests as searchWorkspaceManifests } from './search-workspace.manifests.js';
import { manifests as indexDetailBoxManifests } from './indexDetailBoxes.manifests.js';
import { manifest as indexDetailBoxKindManifest } from './indexDetailBoxKind.manifests.js';

import { UMB_SEARCH_ROOT_ENTITY_TYPE } from '@umbraco-cms/search/global';
import { UMB_ADVANCED_SETTINGS_MENU_ALIAS } from '@umbraco-cms/backoffice/settings';

// Job of the bundle is to collate all the manifests from different parts of the extension and load other manifests
// We load this bundle from umbraco-package.json
export const manifests: Array<UmbExtensionManifest> = [
  ...globalContextManifests,
  ...localizationManifests,
  ...repositoryManifests,
  ...collectionActionManifests,
  ...collectionViewManifests,
  ...conditionManifests,
  ...entityActionManifests,
  ...searchRootWorkspaceManifests,
  ...searchWorkspaceManifests,
  ...indexDetailBoxManifests,
  indexDetailBoxKindManifest as never,
  {
    type: 'menuItem',
    name: 'Umbraco Search Root Menu Item',
    alias: 'Umbraco.Search.Root.MenuItem',
    meta: {
      label: '#search_treeHeader',
      entityType: UMB_SEARCH_ROOT_ENTITY_TYPE,
      icon: 'icon-search',
      menus: [UMB_ADVANCED_SETTINGS_MENU_ALIAS],
    },
  },
];
