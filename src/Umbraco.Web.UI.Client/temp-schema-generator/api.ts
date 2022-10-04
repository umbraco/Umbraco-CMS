import './installer';
import './manifests';
import './publishedstatus';
import './server';
import './upgrader';
import './user';
import './property-editors';

import { api } from '@airtasker/spot';

/* eslint-disable */
@api({ name: 'umbraco-backoffice-api', version: '1.0.0' })
class Api {}
