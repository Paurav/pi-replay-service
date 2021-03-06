#PI Replay

This application is developped to replay existing data from an existing PI Data Archive server. 
The main principle is that older data is inserted in the snapshot ( present value), to simulate a real time data flow based on historical data.
The older data is determined based on a configurable time offset in days, default is 365.

It functions in the following way:
- Load Tags: On start, the application will load all the tags who are matching the tag query that is configured
- Get Current Values (Snapshots): the application will initially get all the snapshot values and memorize the most recent, this will become the start time of the recovery period.
- Recovery/Backfill: from the previous step, the application will divide the time periods in (configurable) smaller time ranges, query those time ranges, and insert the data in the snapshot until it reaches the backfill endtime ( the time at which the backfill started)
- Normal/operation: calls are made every X seconds where data is taken in history and insertd in the snapshot.  The time range here is only few seconds, thus this takes much less time and memory to run.


# Installation and Executables

The build of this application, use the Build folder at the root of the solution,  comes with a series of executable:
- PIReplayCommandLine:        this is the command line version.  It does the exact same thing as the service, except it also has an option to delete data, useful to remove some of the most recent data that may be in a bad state if the server was running disconnected for a while before performing the backup.
- PIReplayService.exe:        executable for the service.  To install: open the command as administrator, navigate to the folder e.g. "cd /d c:\your_folder\replay-service".  Then run the command: PIReplayService.exe --install. to uninstall the service, just run the uninstall command.  The only thing it does it registers the service in windows. You could achive the same by using the sc create windows command. This is just easier.  
- PIReplayServiceConfigurator: User interface.  Once the service is installed, this UI allows to stop/start the service.  Modify and save the settings ( require a service restart to be applied ). View service logs with a small utility called baretail, this is very convenient.


# Discussion on the stream process

One of the challenges while writing such application is that you want to both maximize the speed, this is done by using the very efficient PI AF SDK Bulk calls. 
However, this returns quite a lot of data and you need to get this data flowing so memory does not fills up with all the records. 
To deal with this, this application makes data calls in small(er) chunks, and this allows to help the data flowing.

# Todo

done - Remove: delete values using the old fashion method. if cant make it work with the classic method.
- refactor the read data class, this is a mess: split the backfill and the read data, or find a way to remove all ifs... 
- Cleanup logs and make them more comprehensive


#Settings example

#Settings sample:
  <PIReplay.Settings.General>
        <setting name="ReplayTimeOffsetDays" serializeAs="String">
            <value>365</value>
        </setting>
        <setting name="BackFillHoursPerDataChunk" serializeAs="String">
            <value>24</value>
        </setting>
        <setting name="TagsChunkSize" serializeAs="String">
            <value>500</value>
        </setting>
        <setting name="DataCollectionFrequencySeconds" serializeAs="String">
            <value>5</value>
        </setting>
        <setting name="ServerName" serializeAs="String">
            <value>pidemo2016</value>
        </setting>
        <setting name="TagQueryString" serializeAs="String">
            <value>tag:=simulator.random.*</value>
        </setting>
        <setting name="BulkPageSize" serializeAs="String">
            <value>1000</value>
        </setting>
        <setting name="BulkParallelChunkSize" serializeAs="String">
            <value>100</value>
        </setting>
        <setting name="BackfillDefaultStartTime" serializeAs="String">
            <value>08/12/2016 17:00:00</value>
        </setting>
        <setting name="SleepTimeBetweenChunksMs" serializeAs="String">
            <value>200</value>
        </setting>

#License
 
    Copyright 2015 Patrice Thivierge Fortin
 
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
 
    http://www.apache.org/licenses/LICENSE-2.0
 
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
