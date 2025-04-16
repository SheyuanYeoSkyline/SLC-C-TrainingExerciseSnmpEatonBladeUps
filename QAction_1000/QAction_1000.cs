using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: interfaceTableUpdated.
/// </summary>
public static class QAction
{
    /// <summary>
    /// Computes the interface table speed of a given row in the interface table, in megabits per second.
    /// Assumes that the extended interface table is populated if necessary.
    /// </summary>
    /// <param name="protocol">Protocol containing the table.</param>
    /// <param name="row">Row of the interface table.</param>
    /// <returns>Current speed in megabits per second.</returns>
    private static double GetInterfaceTableSpeed(SLProtocolExt protocol, InterfacetableQActionRow row)
    {
        var interfaceTableSpeedRawValue = row.Interfacetablespeed_1003;
        double interfaceTableInterfaceSpeedMbps;

        var useExtendedTableSpeed = (uint)(double)interfaceTableSpeedRawValue == uint.MaxValue;
        if (useExtendedTableSpeed)
        {
            var extendedInterfaceTable = protocol.extendedinterfacetable;
            var extendedTableRow = new ExtendedinterfacetableQActionRow(extendedInterfaceTable[row.Key]);
            var extendedSpeed = (double)extendedTableRow.Extendedinterfacetableextendedspeed_2002;
            interfaceTableInterfaceSpeedMbps = extendedSpeed;
        }
        else
        {
            var interfaceTableSpeedMbps = (double)interfaceTableSpeedRawValue / 10e6;
            interfaceTableInterfaceSpeedMbps = interfaceTableSpeedMbps;
        }

        return interfaceTableInterfaceSpeedMbps;
    }

    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            var interfaceTable = protocol.interfacetable;
            List<(string key, double speed)> speeds = interfaceTable.Keys.Select(key => new InterfacetableQActionRow(interfaceTable[key]))
                .Select(row => (row.Key, GetInterfaceTableSpeed(protocol, row)))
                .ToList();
            interfaceTable.SetColumn(Parameter.Interfacetable.Pid.interfacetableinterfacespeed_1005, speeds.Select(x => x.key).ToArray(), speeds.Select(x => (object)x.speed).ToArray());
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}